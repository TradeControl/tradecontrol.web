import { defineConfig, devices } from "@playwright/test";

const baseUrl = process.env.BASE_URL || "https://localhost:5001";
const ignoreHttpsErrors = true;

export default defineConfig({
  testDir: "./tests",
  timeout: 60_000,
  expect: { timeout: 10_000 },
  reporter: [
    ["list"],
    ["html", { open: "never", outputFolder: "playwright-report" }]
  ],
  use: {
    baseURL: baseUrl,
    ignoreHTTPSErrors: ignoreHttpsErrors,
    trace: "on",
    screenshot: "only-on-failure",
    video: "retain-on-failure"
  },
  projects: [
    { name: "chromium", use: { ...devices["Desktop Chrome"] } }
  ]
});
