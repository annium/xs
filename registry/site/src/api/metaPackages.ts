import { HttpResponse } from '@annium/client-http'
import { identity, pickBy } from 'lodash'

import { MetaPackageData, toMetaPackage } from '../models/data/MetaPackageData'
import { MetaPackage } from '../models/view/MetaPackage'
import { MetaPackagePermission } from '../models/view/MetaPackagePermission'

import { api } from './api'


export const search = async (
  ownerId: string,
  type: string,
  query: string,
  page: number,
): Promise<HttpResponse<MetaPackage[]>> => {
  const q = pickBy({ ownerId, type, query: encodeURIComponent(query), page }, identity)

  return (await api.get<MetaPackageData[]>('packages/search', q)).map(data => data.map(toMetaPackage))
}

export const get = async (type: string, name: string): Promise<HttpResponse<MetaPackage | undefined>> => {
  const packageName = encodeURIComponent(name)

  return (await api.get<MetaPackageData>(`packages/${type}/${packageName}`)).map(toMetaPackage)
}

export const setPermissions = async (
  type: string, name: string,
  permissions: MetaPackagePermission[],
): Promise<HttpResponse> =>
  api.post(`packages/${type}/${encodeURIComponent(name)}/permissions`, undefined, permissions)
