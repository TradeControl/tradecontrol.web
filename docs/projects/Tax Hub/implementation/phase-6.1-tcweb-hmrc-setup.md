# Phase 6.1 TCWeb HMRC sandbox setup

This setup enables the product-owned HMRC connection inside the authenticated VAT workspace. The separate Tax Hub WebHarness remains a diagnostic tool and is not part of this route.

## HMRC application

Register the exact callback formed from the configured public origin and callback path. Examples are:

- local TCWeb: `https://localhost:44381/TaxHub/HmrcCallback`
- hosted TCWeb: `https://<tcweb-app>.azurewebsites.net/TaxHub/HmrcCallback`

The user connects with the Government Gateway **organisation** account that owns the VAT (MTD) registration. Trade Control does not request, receive or store the Government Gateway user ID or password.

One consent requests `read:vat write:vat`. The encrypted principal grant is independent of the ASP.NET Identity cookie: signing out of Trade Control retains it, while **Disconnect HMRC** retires it.

## Local development

The checked-in Development profile enables the sandbox product boundary and fixes its callback at `https://localhost:44381/TaxHub/HmrcCallback`. Development composition derives the ignored store and credential paths from the repository root:

- `.local/tax-hub/tcweb`
- `.local/vat_mtd_client_test-master/mtd-client-vat/clientsettings.json`

No credential value or machine-specific absolute path is committed. Register the callback above in the HMRC sandbox application, restart TCWeb, then use **Connect HMRC** in the Tax Hub left panel.

Strict fraud capture is disabled for the localhost profile because a loopback socket cannot truthfully supply the required public client/server topology. It must be enabled with reviewed public TLS hops and trusted proxy peers before an interactive HMRC API call is exposed. OAuth connection testing does not manufacture public network facts.

The equivalent explicit settings are:

```json
{
  "TaxHub": {
    "VatProduct": {
      "Enabled": true,
      "AuthorityEnvironment": "Sandbox",
      "PersistenceMode": "DevelopmentFiles",
      "TenantReference": "<opaque-node-guid>",
      "PublicOrigin": "https://localhost:44381/",
      "OAuthCallbackPath": "/TaxHub/HmrcCallback",
      "DevelopmentStoreRoot": "<absolute-git-ignored-directory>",
      "SandboxSecretSource": "DevelopmentJsonFile",
      "DevelopmentClientSettingsPath": "<absolute-path-to-clientsettings.json>",
      "FraudCaptureEnabled": true,
      "FraudPublicTlsAddresses": [ "<public-address-of-the-TLS-hop>" ],
      "FraudTrustedProxyAddresses": []
    }
  }
}
```

For a hosted sandbox, select `EnvironmentVariables`, omit `DevelopmentClientSettingsPath`, enable fraud capture, and supply the protected App Service settings `TaxHub__HmrcSandbox__ClientId` and `TaxHub__HmrcSandbox__ClientSecret`. Their values must never appear in JSON committed to the repository, command output, logs or documentation.

For a reviewed reverse-proxy deployment, list every public TLS hop in order and allow-list the exact immediate proxy peers in `FraudTrustedProxyAddresses`. Forwarded client data is rejected unless the socket peer is in that list. A direct deployment requires exactly one public TLS address.

`DevelopmentFiles` is deliberately rejected outside the Development environment. Production HMRC and Azure-managed persistence remain disabled pending the separate production review recorded in Phase 6.0.

## Product endpoints

- `GET /TaxHub/Hmrc/Status` — safe connection state only
- `GET /TaxHub/Hmrc/Connect` — begins the state-bound PKCE journey
- `GET /TaxHub/Hmrc/Reauthorise` — begins a replacement consent journey
- `GET /TaxHub/HmrcCallback` — fixed registered callback; no caller-controlled return URL
- `POST /TaxHub/Hmrc/Disconnect` — retires the stored grant
- `POST /TaxHub/Hmrc/ClientFacts` — accepts browser facts only; tenant, actor and ingress facts are derived by TCWeb

All endpoints require ASP.NET Identity. Connect, reauthorise, disconnect and browser capture additionally apply the Phase 6.1 Administrators/Managers filing policy. This is a narrow product policy, not part of the transport-neutral VAT workflow contract.
