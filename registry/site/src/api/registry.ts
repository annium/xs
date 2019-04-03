import { Response } from '@annium/server-http'
import _ from 'lodash'

import { api } from './api'


export const load = async (): Promise<Response<{ [key: string]: URL }>> =>
  (await api.get<{ [key: string]: string }>('registry')).map(data => _.mapValues(data, url => new URL(url)))

