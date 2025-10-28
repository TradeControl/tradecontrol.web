// Flat config for ESLint v9+
// Enforces Allman braces and mandatory curly blocks, targets your wwwroot/js files.
// Adjust the globs if your solution folder name changes.

/** @type {import('eslint').Linter.FlatConfig[]} */
module.exports = [
  {
    files: ["**/wwwroot/js/**/*.js"],
    ignores: ["**/bin/**", "**/obj/**", "**/wwwroot/lib/**"],
    languageOptions: {
      ecmaVersion: 2022,
      sourceType: "script",
      globals: {
        window: "readonly",
        document: "readonly",
        navigator: "readonly",
        $: "readonly",
        jQuery: "readonly",
      },
    },
    rules: {
      "brace-style": ["error", "allman", { "allowSingleLine": true }],
      "curly": ["error", "all"],
    },
  },
];