import _ from 'lodash'
import { Response } from 'site.lib/dist/api'

import MetaPackageData, { toMetaPackage } from '../models/data/MetaPackageData'
import MetaPackage from '../models/view/MetaPackage'
import MetaPackagePermission from '../models/view/MetaPackagePermission'

import api from './api'


export default {
  async search(ownerId: string, type: string, query: string, page: number): Promise<Response<MetaPackage[]>> {
    const q = _.pickBy({ ownerId, type, query: encodeURIComponent(query), page }, _.identity)
    const { data, error } = await api.get<MetaPackageData[]>('packages/search', q)

    return new Response(data.map(toMetaPackage), error)
  },
  async get(type: string, name: string): Promise<Response<MetaPackage | null>> {
    name = encodeURIComponent(name)
    const { data, error } = await api.get<MetaPackageData>(`packages/${type}/${name}`)

    return new Response(data ? toMetaPackage(data) : null, error)
  },
  async setPermissions(type: string, name: string, permissions: MetaPackagePermission[]): Promise<Response> {
    name = encodeURIComponent(name)
    return await api.post(`packages/${type}/${name}/permissions`, undefined, permissions)
  },
}