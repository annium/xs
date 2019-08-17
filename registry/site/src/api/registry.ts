import { HttpResponse } from '@annium/client-http'

import { api } from './api'
import { Registry } from '../models/view/Registry'
import { RegistryData, toRegistryData as toRegistry } from '../models/data/RegistryData'


export const load = async (): Promise<HttpResponse<Registry>> => {
  const raw = await api.get<RegistryData>('registry')

  return raw.map(toRegistry)
}
