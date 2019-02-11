import _ from 'lodash'
import { when } from 'mobx'
import * as lib from 'site.lib'
import { Response } from 'site.lib/dist/api'

import { store } from '../..'
import PackageData from '../../models/data/PackageData'
import Package from '../../models/view/Package'
import { ProjectType } from '../../models/view/ProjectType'


export default function createApi<TPackageData extends PackageData, TPackage extends Package>(
  type: ProjectType,
  getTokenHeader: (token: string) => Record<string, string>,
  toPackage: (data: TPackageData) => TPackage
) {
  return {
    async get(name: string): Promise<Response<TPackage[]>> {
      const api = await getApi(type, getTokenHeader)

      name = encodeURIComponent(name)
      const { data, error } = await api.get<TPackageData[]>(`packages/${name}`)

      return new Response(data.map(toPackage), error)
    },
    async delete(name: string, version: string): Promise<Response> {
      const api = await getApi(type, getTokenHeader)

      name = encodeURIComponent(name)
      return await api.delete(`packages/${name}/${version}`)
    },
  }
}

async function getApi(
  type: ProjectType,
  getTokenHeader: (token: string) => Record<string, string>
): Promise<lib.api.Client> {
  await when(() => Boolean(store))
  await when(() => Boolean(store.user.data))

  const { servers } = store.startup
  const { apiToken } = store.user.data!

  const server = servers[type]

  if (!server) throw new Error('Server is not registered')

  return lib.api.factory({
    url: server,
    init: {
      headers: getTokenHeader(apiToken),
    },
  })
}