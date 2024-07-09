import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import simplePlantUML from '@akebifiky/remark-simple-plantuml';

// https://astro.build/config
export default defineConfig({
  outDir: '../dist/docs',
  site: 'https://froko.github.io',
  base: 'simple-domain',
  markdown: {
    remarkPlugins: [simplePlantUML]
  },
  integrations: [
    starlight({
      title: 'SimpleDomain',
      social: {
        github: 'https://github.com/froko/simple-domain'
      },
      sidebar: [
        {
          label: 'Getting Started',
          link: 'getting-started'
        },
        {
          label: 'Documentation',
          autogenerate: { directory: 'documentation' }
        },
        {
          label: 'References',
          link: 'references'
        }
      ]
    })
  ]
});
