import { factory } from '@annium/server-http'

export const api = factory({
  url: {
    host: process.env.REACT_APP_API_HOST || location.hostname,
    port: parseInt(process.env.REACT_APP_API_PORT || location.port, 10),
    protocol: process.env.REACT_APP_API_PROTOCOL || location.protocol,
  },
})
