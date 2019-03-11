import { Response } from '@xs/site.lib/dist/api'
import _ from 'lodash'

import api from './api'


export default {
  async load(): Promise<Response<{ [key: string]: URL }>> {
    const response = await api.get<{ [key: string]: string }>('registry')

    return new Response(_.mapValues(response.data, url => new URL(url)), response.error)
  },
}