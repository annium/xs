import { httpClientFactory } from '@annium/client-http'

const { protocol, hostname: host, port } = new URL(process.env.REACT_APP_API || window.location.toString())

export const api = httpClientFactory({
  url: {
    protocol,
    // tslint:disable-next-line: object-literal-sort-keys
    host,
    port: parseInt(port, 10),
  },
})
