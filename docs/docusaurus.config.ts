import {themes as prismThemes} from 'prism-react-renderer';
import type {Config} from '@docusaurus/types';
import type * as Preset from '@docusaurus/preset-classic';

const config: Config = {
  title: 'Cosmigrator',
  tagline: 'Migration framework for Azure Cosmos DB',
  favicon: 'img/favicon.ico',

  future: {
    v4: true,
  },

  url: 'https://cosmigrator.dev',
  baseUrl: '/',

  organizationName: 'AdelSS04',
  projectName: 'Cosmigrator',

  onBrokenLinks: 'throw',

  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

  presets: [
    [
      'classic',
      {
        docs: {
          sidebarPath: './sidebars.ts',
          routeBasePath: '/',
          editUrl:
            'https://github.com/AdelSS04/Cosmigrator/tree/master/docs/',
        },
        blog: false,
        theme: {
          customCss: './src/css/custom.css',
        },
      } satisfies Preset.Options,
    ],
  ],

  themeConfig: {
    colorMode: {
      defaultMode: 'dark',
      respectPrefersColorScheme: true,
    },
    navbar: {
      title: 'Cosmigrator',
      items: [
        {
          type: 'docSidebar',
          sidebarId: 'docsSidebar',
          position: 'left',
          label: 'Docs',
        },
        {
          href: 'https://www.nuget.org/packages/Cosmigrator/',
          label: 'NuGet',
          position: 'right',
        },
        {
          href: 'https://github.com/AdelSS04/Cosmigrator',
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Docs',
          items: [
            {label: 'Getting Started', to: '/getting-started/installation'},
            {label: 'Core Concepts', to: '/core-concepts/how-it-works'},
            {label: 'API Reference', to: '/api-reference/imigration'},
          ],
        },
        {
          title: 'Links',
          items: [
            {label: 'GitHub', href: 'https://github.com/AdelSS04/Cosmigrator'},
            {label: 'NuGet', href: 'https://www.nuget.org/packages/Cosmigrator/'},
            {label: 'Changelog', href: 'https://github.com/AdelSS04/Cosmigrator/blob/master/CHANGELOG.md'},
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} Cosmigrator Contributors. MIT License.`,
    },
    prism: {
      theme: prismThemes.github,
      darkTheme: prismThemes.dracula,
      additionalLanguages: ['csharp', 'json', 'bash', 'yaml'],
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
