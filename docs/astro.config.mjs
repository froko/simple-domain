import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import simplePlantUML from '@akebifiky/remark-simple-plantuml';

// https://astro.build/config
export default defineConfig({
  outDir: '../dist/docs',
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
          label: 'References',
          autogenerate: { directory: 'references' }
        }
      ]
    })
  ]
});
