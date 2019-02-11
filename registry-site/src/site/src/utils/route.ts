export default {
  package: (type: string, name: string, version?: string) => {
    return '/' + ['packages', type, name, version].filter(e => e).join('/')
  },
}