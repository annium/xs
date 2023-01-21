export const pkg = (type: string, name: string, version?: string) =>
  `/${['packages', type, name, version].filter(e => e).join('/')}`
