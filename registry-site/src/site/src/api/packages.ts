import { Response } from 'site.lib/dist/api'

import MetaPackageData, { toMetaPackage } from '../models/data/MetaPackageData'
import MetaPackage from '../models/view/MetaPackage'

import api from './api'


export default {
  async my(): Promise<Response<MetaPackage[]>> {
    const { data, error } = await api.get<MetaPackageData[]>('packages/my')

    return new Response(data.map(toMetaPackage), error)
  },
  async search(query: string, page: number): Promise<Response<MetaPackage[]>> {
    const { data, error } = await api.get<MetaPackageData[]>('packages/search', { query, page })

    return new Response(data.map(toMetaPackage), error)
  },
  async get(type: string, query: string): Promise<Response<MetaPackage[]>> {
    const { data, error } = await api.get<MetaPackageData[]>(`${type}/${name}`)

    return new Response(data.map(toMetaPackage), error)
  },
}