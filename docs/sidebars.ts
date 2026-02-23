import type {SidebarsConfig} from '@docusaurus/plugin-content-docs';

const sidebars: SidebarsConfig = {
  docsSidebar: [
    'intro',
    {
      type: 'category',
      label: 'Getting Started',
      items: [
        'getting-started/installation',
        'getting-started/quick-start',
        'getting-started/cli-commands',
      ],
    },
    {
      type: 'category',
      label: 'Core Concepts',
      items: [
        'core-concepts/how-it-works',
        'core-concepts/migration-lifecycle',
        'core-concepts/history-tracking',
      ],
    },
    {
      type: 'category',
      label: 'Migration Scenarios',
      items: [
        'migrations/add-property',
        'migrations/remove-property',
        'migrations/rename-property',
        'migrations/unique-key-policy',
        'migrations/composite-index',
      ],
    },
    {
      type: 'category',
      label: 'Integrations',
      items: [
        'integrations/kubernetes',
        'integrations/ci-cd',
        'integrations/custom-host',
      ],
    },
    {
      type: 'category',
      label: 'API Reference',
      items: [
        'api-reference/imigration',
        'api-reference/migration-host',
        'api-reference/migration-runner',
        'api-reference/migration-history',
        'api-reference/bulk-operation-helper',
        'api-reference/migration-discovery',
        'api-reference/migration-record',
      ],
    },
  ],
};

export default sidebars;
