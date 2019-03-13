import * as lib from '@xs/site.lib'

export const api = lib.api.factory({
  url: {
    host: process.env.REACT_APP_API_HOST || location.hostname,
    port: parseInt(process.env.REACT_APP_API_PORT || location.port, 10),
    protocol: process.env.REACT_APP_API_PROTOCOL || location.protocol,
  },
})
