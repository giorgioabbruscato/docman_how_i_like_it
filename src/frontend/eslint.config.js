import js from '@eslint/js';
import reactPlugin from 'eslint-plugin-react';
import tseslint from 'typescript-eslint';

export default tseslint.config(
  js.configs.recommended,
  ...tseslint.configs.recommended,
  {
    files: ['**/*.{ts,tsx}'],
    plugins: {
      react: reactPlugin,
    },
    languageOptions: {
      parserOptions: {
        ecmaFeatures: {
          jsx: true,
        },
      },
    },
    settings: {
      react: {
        version: 'detect',
      },
    },
    rules: {
      'react/no-danger': 'error',
    },
  },
  {
    ignores: ['dist/**', 'node_modules/**'],
  },
);
