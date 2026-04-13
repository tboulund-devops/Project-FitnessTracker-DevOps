import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import path from 'path';
import { fileURLToPath } from 'url';

const configDir = path.dirname(fileURLToPath(import.meta.url));

const cssStubPlugin = {
  name: 'css-stub-plugin',
  enforce: 'pre' as const,
  resolveId(source: string) {
    if (source.endsWith('.css')) {
      return '\0css-stub';
    }
    return null;
  },
  load(id: string) {
    if (id === '\0css-stub') {
      return 'export default {}';
    }
    return null;
  }
};

export default defineConfig({
  plugins: [cssStubPlugin, react()],
  resolve: {
    dedupe: ['react', 'react-dom'],
    alias: [
      {
        find: /^react$/,
        replacement: path.resolve(configDir, 'node_modules/react/index.js')
      },
      {
        find: /^react\/jsx-runtime$/,
        replacement: path.resolve(configDir, 'node_modules/react/jsx-runtime.js')
      },
      {
        find: /^react\/jsx-dev-runtime$/,
        replacement: path.resolve(configDir, 'node_modules/react/jsx-dev-runtime.js')
      },
      {
        find: /^react-dom$/,
        replacement: path.resolve(configDir, 'node_modules/react-dom/index.js')
      },
      {
        find: /^react-dom\/client$/,
        replacement: path.resolve(configDir, 'node_modules/react-dom/client.js')
      },
      {
        find: /^react-router-dom$/,
        replacement: path.resolve(configDir, 'node_modules/react-router-dom/dist/index.mjs')
      },
      {
        find: /^react-router$/,
        replacement: path.resolve(configDir, 'node_modules/react-router/dist/development/index.mjs')
      }
    ]
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./setupTests.ts'],
    clearMocks: true,
    restoreMocks: true,
    globals: true,
    coverage: {
      provider: 'v8',
      reporter: ['text', 'html', 'lcov'],
      allowExternal: true,
      include: [
        '**/Frontend/src/App.tsx',
        '**/Frontend/src/assets/Components/NavBar.tsx',
        '**/Frontend/src/assets/Services/PageNavigation.tsx',
        '**/Frontend/src/assets/pages/HomePage.tsx',
        '**/Frontend/src/assets/pages/LoginPage.tsx',
        '**/Frontend/src/assets/pages/OldWorkouts.tsx',
        '**/Frontend/src/assets/pages/ProfilePage.tsx',
        '**/Frontend/src/assets/pages/RegistrationPage.tsx',
        '**/Frontend/src/assets/pages/Settings.tsx'
      ],
      thresholds: {
        lines: 100,
        functions: 100,
        statements: 100,
        branches: 100
      }
    }
  }
});
