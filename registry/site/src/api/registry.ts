import { Response } from '@annium/server-http'
import _ from 'lodash'

import { api } from './api'


export const load = async (): Promise<Response<{ [key: string]: URL }>> => {
  const response = await api.get<{ [key: string]: string }>('registry')

  return new Response(_.mapValues(response.data, url => new URL(url)), response.error)
}
