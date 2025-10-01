CREATE   FUNCTION Cash.fnChangeKeyPath (@CoinTypeCode smallint, @HDPath nvarchar(256), @ChangeTypeCode smallint, @AddressIndex int)
RETURNS nvarchar(256)
AS
BEGIN
	DECLARE @KeyPath nvarchar(256) = CONCAT('44', '''', '/', @CoinTypeCode, REPLACE(@HDPath, '/', '''/'), @ChangeTypeCode, '/', @AddressIndex);
	RETURN @KeyPath;
END
