import _ from 'lodash'
import { Response } from 'site.lib/dist/api'

import api from './api'


export default {
  async load(): Promise<Response<{ [key: string]: URL }>> {
    const response = await api.get<{ [key: string]: string }>('registry')

    return new Response(_.mapValues(response.data, url => new URL(url)), response.error)
  },
}