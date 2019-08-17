import * as _ from 'lodash'

import { Registry } from '../view/Registry'

export type RegistryData = {
  servers: Record<string, string>
}

export const toRegistryData = (data: RegistryData): Registry => {
  const servers = _.mapValues(data.servers, value => new URL(value))

  return {
    servers,
  }
}
