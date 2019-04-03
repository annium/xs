import { Client, factory, Response } from '@annium/server-http'
import { when } from '@annium/utils'

import { context } from '../../context'
import { PackageData } from '../../models/data/PackageData'
import { Package } from '../../models/view/Package'
import { ProjectType } from '../../models/view/ProjectType'


export function createApi<TPackageData extends PackageData, TPackage extends Package>(
  type: ProjectType,
  getTokenHeader: (token: string) => Record<string, string>,
  toPackage: (data: TPackageData) => TPackage,
) {
  return {
    async get(name: string): Promise<Response<TPackage[]>> {
      const api = await getApi(type, getTokenHeader)

      const packageName = encodeURIComponent(name)

      return (await api.get<TPackageData[]>(`packages/${packageName}`)).map(data => data.map(toPackage))
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
): Promise<Client> {
  await when(() => Boolean(context.getState().auth.user.data))

  const { servers } = context.getState().startup
  const { apiToken } = context.getState().auth.user.data!

  const server = servers[type]

  if (!server) throw new Error('Server is not registered')

  return factory({
    url: server,
    init: {
      headers: getTokenHeader(apiToken),
    },
  })
}
