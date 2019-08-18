import { HttpResponse } from '@annium/client-http'

import { RegistryData, toRegistryData as toRegistry } from '../models/data/RegistryData'
import { Registry } from '../models/view/Registry'

import { api } from './api'


export const load = async (): Promise<HttpResponse<Registry>> => {
  const raw = await api.get<RegistryData>('registry')

  return raw.map(toRegistry)
}
