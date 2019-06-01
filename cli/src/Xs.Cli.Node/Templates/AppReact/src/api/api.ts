import { factory } from '@annium/server-http'

const { protocol, hostname: host, port } = new URL(process.env.REACT_APP_API_URL || window.location.toString())

export const api = factory({
  url: {
    protocol,
    // tslint:disable-next-line: object-literal-sort-keys
    host,
    port: parseInt(port, 10),
  },
})
