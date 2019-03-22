import * as lib from '@xs/site.lib'
import { Response } from '@xs/site.lib/dist/api'
import _ from 'lodash'
import { when } from 'mobx'

import { PackageData } from '../../models/data/PackageData'
import { Package } from '../../models/view/Package'
import { ProjectType } from '../../models/view/ProjectType'
import { User } from '../../models/view/User'
import { store } from '../../store'


export function createApi<TPackageData extends PackageData, TPackage extends Package>(
  type: ProjectType,
  getTokenHeader: (token: string) => Record<string, string>,
  toPackage: (data: TPackageData) => TPackage,
) {
  return {
    async get(name: string): Promise<Response<TPackage[]>> {
      const api = await getApi(type, getTokenHeader)

      const packageName = encodeURIComponent(name)
      const { data, error } = await api.get<TPackageData[]>(`packages/${packageName}`)

      return new Response(data.map(toPackage), error)
    },
    async delete(name: string, version: string): Promise<Response> {
      const api = await getApi(type, getTokenHeader)

      const packageName = encodeURIComponent(name)

      return api.delete(`packages/${packageName}/${version}`)
    },
  }
}

async function getApi(
  type: ProjectType,
  getTokenHeader: (token: string) => Record<string, string>,
): Promise<lib.api.Client> {
  await when(() => Boolean(store))
  await when(() => Boolean(store.auth.data))

  const { servers } = store.startup
  const { apiToken } = store.auth.data as User

  const server = servers[type]

  if (!server) throw new Error('Server is not registered')

  return lib.api.factory({
    url: server,
    init: {
      headers: getTokenHeader(apiToken),
    },
  })
}
