using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TradeControl.Web.Mail;

namespace TradeControl.Web.Data
{
    public class NodeSettings
    {
        readonly NodeContext _context;

        public NodeSettings(NodeContext context) => _context = context;

        #region web install
        public bool IsInitialised
        {
            get
            {
                try
                {
                    if (!_context.App_tbOptions.Any())
                        return false;
                    else
                        return _context.App_tbOptions.First().IsInitialised;
                }
                catch (Exception e)
                {
                    _ = _context.ErrorLog(e);
                    return false;
                }
            }
            set
            {
                try
                {
                    int result = _context.Database.ExecuteSqlRaw("App.proc_Initialised @p0", parameters: new[] { value });
                }
                catch (Exception e)
                {
                    _ = _context.ErrorLog(e);
                }
            }
        }

        public bool IsFirstUse
        {
            get
            {
                try
                {
                    return !_context.AspNet_UserRegistrations.Any();
                }
                catch (Exception e)
                {
                    _ = _context.ErrorLog(e);
                    return false;
                }
            }
        }

        public bool HasMailHost
        {
            get
            {
                try
                {
                    return _context.App_Host.Any();
                }
                catch (Exception e)
                {
                    _ = _context.ErrorLog(e);
                    return false;
                }
            }
        }

        public Task<double> DataVersion => Task.Run(() =>
        {
            try
            {
                return _context.App_Version.First().SqldataVersion;
            }
            catch (Exception e)
            {
                _ = _context.ErrorLog(e);
                return 0;
            }
        });

        public Task<bool> IsPeriodEnd => Task.Run(() =>
        {
            try
            {
                if (!_context.App_ActivePeriods.Any())
                    return false;
                else
                    return _context.App_ActivePeriods.First().EndOn < DateTime.Today;
            }
            catch (Exception e)
            {
                _ = _context.ErrorLog(e);
                return false;
            }
        });

        public Task<NodeEnum.CoinType> CoinType => Task.Run(() =>
        {
            try
            {
                return (NodeEnum.CoinType)_context.App_tbOptions.First().CoinTypeCode;
            }
            catch (Exception e)
            {
                _ = _context.ErrorLog(e);
                return NodeEnum.CoinType.Fiat;
            }

        });
        #endregion

        #region mail
        /// <summary>
        /// Retrieve symmetric key and IV from App.tbOptions, creating them if missing.
        /// Returns (Key, IV) where Key is 32 bytes (AES-256) and IV is 16 bytes.
        /// </summary>
        public async Task<(byte[] Key, byte[] IV)> GetOrCreateSymmetricAsync()
        {
            try
            {
                var options = await _context.App_tbOptions.FirstOrDefaultAsync() ?? throw new InvalidOperationException("App.tbOptions not initialized.");
                bool changed = false;

                if (options.SymmetricKey == null || options.SymmetricKey.Length == 0)
                {
                    options.SymmetricKey = RandomNumberGenerator.GetBytes(32); // AES-256
                    changed = true;
                }

                if (options.SymmetricIV == null || options.SymmetricIV.Length == 0)
                {
                    options.SymmetricIV = RandomNumberGenerator.GetBytes(16); // AES block size IV
                    changed = true;
                }

                if (changed)
                {
                    _context.Attach(options).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }

                return (options.SymmetricKey, options.SymmetricIV);
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                throw;
            }
        }

        /// <summary>
        /// Regenerate (rotate) symmetric key + IV and re-encrypt App_Host.EmailPassword values.
        /// Workflow:
        /// 1. Read current key/iv from App.tbOptions (old).
        /// 2. Call stored proc App.proc_RotateSymmetricKeys passing the old key/iv for verification
        ///    (proc generates & stores new key/iv when NewKey/NewIV are NULL).
        /// 3. Read new key/iv from App.tbOptions.
        /// 4. For each host: decrypt using old key/iv, encrypt using new key/iv and persist.
        /// Note: this operation is destructive if interrupted — back up DB before running.
        /// </summary>
        public async Task<bool> RegenerateSymmetricAsync()
        {
            try
            {
                var options = await _context.App_tbOptions.FirstOrDefaultAsync() ?? throw new InvalidOperationException("App.tbOptions not initialized.");

                byte[] oldKey = options.SymmetricKey;
                byte[] oldIV  = options.SymmetricIV;

                if (oldKey == null || oldIV == null)
                {
                    // If no existing key/iv, simply call proc to create them and nothing to re-encrypt.
                    await using var conn0 = _context.Database.GetDbConnection();
                    await conn0.OpenAsync();
                    await using var cmd0 = conn0.CreateCommand();
                    cmd0.CommandText = "App.proc_RotateSymmetricKeys";
                    cmd0.CommandType = CommandType.StoredProcedure;
                    // pass NULL for OldKeyInput to allow initial creation
                    var pNewKey0 = cmd0.CreateParameter(); pNewKey0.ParameterName = "@NewKey"; pNewKey0.Value = DBNull.Value; cmd0.Parameters.Add(pNewKey0);
                    var pNewIV0 = cmd0.CreateParameter();  pNewIV0.ParameterName  = "@NewIV";  pNewIV0.Value  = DBNull.Value; cmd0.Parameters.Add(pNewIV0);
                    await cmd0.ExecuteNonQueryAsync();
                    return true;
                }

                // Call proc to rotate keys, verifying caller knows old values.
                await using var conn = _context.Database.GetDbConnection();
                await conn.OpenAsync();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "App.proc_RotateSymmetricKeys";
                cmd.CommandType = CommandType.StoredProcedure;

                // input params
                var pNewKey = cmd.CreateParameter(); pNewKey.ParameterName = "@NewKey"; pNewKey.Value = DBNull.Value; pNewKey.DbType = DbType.Binary; cmd.Parameters.Add(pNewKey);
                var pNewIV  = cmd.CreateParameter(); pNewIV.ParameterName  = "@NewIV";  pNewIV.Value  = DBNull.Value; pNewIV.DbType  = DbType.Binary; cmd.Parameters.Add(pNewIV);

                var pOldKeyIn = cmd.CreateParameter(); pOldKeyIn.ParameterName = "@OldKeyInput"; pOldKeyIn.Value = oldKey; pOldKeyIn.DbType = DbType.Binary; pOldKeyIn.Size = 32; cmd.Parameters.Add(pOldKeyIn);
                var pOldIVIn  = cmd.CreateParameter(); pOldIVIn.ParameterName  = "@OldIVInput";  pOldIVIn.Value  = oldIV;  pOldIVIn.DbType  = DbType.Binary; pOldIVIn.Size = 16; cmd.Parameters.Add(pOldIVIn);

                // output params (proc returns previous values — not strictly needed here but kept for completeness)
                var pOldKeyOut = cmd.CreateParameter(); pOldKeyOut.ParameterName = "@OldKey"; pOldKeyOut.Direction = ParameterDirection.Output; pOldKeyOut.DbType = DbType.Binary; pOldKeyOut.Size = 32; cmd.Parameters.Add(pOldKeyOut);
                var pOldIVOut  = cmd.CreateParameter(); pOldIVOut.ParameterName  = "@OldIV";  pOldIVOut.Direction = ParameterDirection.Output; pOldIVOut.DbType  = DbType.Binary; pOldIVOut.Size = 16; cmd.Parameters.Add(pOldIVOut);

                // return value param
                var returnParam = cmd.CreateParameter();
                returnParam.ParameterName = "@RETURN_VALUE";
                returnParam.Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(returnParam);

                await cmd.ExecuteNonQueryAsync();

                int rc = returnParam.Value != DBNull.Value ? Convert.ToInt32(returnParam.Value) : -1;
                if (rc != 0)
                {
                    // handle failure: log, surface to admin, abort migration
                    await _context.ErrorLog(new Exception($"proc_RotateSymmetricKeys returned {rc}"));
                    return false;
                }

                // Read new key/iv from DB (proc updated them)
                var updatedOptions = await _context.App_tbOptions.FirstOrDefaultAsync();
                if (updatedOptions == null || updatedOptions.SymmetricKey == null || updatedOptions.SymmetricIV == null)
                    throw new InvalidOperationException("proc_RotateSymmetricKeys did not produce new key/iv.");

                byte[] newKey = updatedOptions.SymmetricKey;
                byte[] newIV  = updatedOptions.SymmetricIV;

                // Re-encrypt App_Host.EmailPassword entries
                // NOTE: App_vwHost is a keyless view type and cannot be tracked/updated.
                // Use the underlying table entity App_tbHost (which has a primary key) for updates.
                var hosts = await _context.App_tbHosts.Where(h => !string.IsNullOrEmpty(h.EmailPassword)).ToListAsync();
                var encryptOld = new Encrypt(oldKey, oldIV);
                var encryptNew = new Encrypt(newKey, newIV);
                bool anyUpdated = false;

                foreach (var host in hosts)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(host.EmailPassword))
                            continue;

                        // decrypt with old key and re-encrypt with new key
                        string plain = encryptOld.DecryptString(host.EmailPassword);
                        // if decryption failed plain may be empty; choose to skip to avoid overwriting valid password with empty
                        if (string.IsNullOrEmpty(plain))
                        {
                            // log and skip this host — administrator may need to intervene
                            await _context.ErrorLog(new Exception($"RegenerateSymmetricAsync: failed to decrypt App_tbHost.HostId={host.HostId}"));
                            continue;
                        }

                        string newCipher = encryptNew.EncryptString(plain);
                        host.EmailPassword = newCipher;
                        _context.Attach(host).State = EntityState.Modified;
                        anyUpdated = true;
                    }
                    catch (Exception exHost)
                    {
                        await _context.ErrorLog(exHost);
                        // continue with other hosts
                    }
                }

                if (anyUpdated)
                    await _context.SaveChangesAsync();

                await _context.EventLog(NodeEnum.EventType.IsInformation, "Symmetric key/IV rotated and host passwords re-encrypted.");
                return true;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }


        public async Task<bool> SetHost(int? hostId)
        {
            try
            {
                var options = await _context.App_tbOptions.FirstAsync();
                options.HostId = hostId;
                _context.Attach(options).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.App_tbOptions.AnyAsync())
                        return false;
                }

                return true;
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return false;
            }
        }

        /// <summary>
        /// Retrieve mail host settings and decrypt stored password using DB-backed symmetric key/IV.
        /// </summary>
        public async Task<MailSettings> MailHost()
        {
            try
            {
                var (key, iv) = await GetOrCreateSymmetricAsync();

                Encrypt encrypt = new(key, iv);
                var defaultHost = await _context.App_Host.OrderBy(h => h.HostId).SingleOrDefaultAsync();

                if (defaultHost == null)
                    return null;

                return new() {
                    HostName = defaultHost.HostName,
                    UserName = defaultHost.EmailAddress,
                    Password = encrypt.DecryptString(defaultHost.EmailPassword),
                    Port = defaultHost.HostPort,
                    IsSmtpAuth = defaultHost.IsSmtpAuth
                };
            }
            catch (Exception e)
            {
                await _context.ErrorLog(e);
                return null;
            }
        }
        #endregion

    }
}

