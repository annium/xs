import { HttpClient, httpClientFactory, HttpResponse } from '@annium/client-http'
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
    async get(name: string): Promise<HttpResponse<TPackage[]>> {
      const api = await getApi(type, getTokenHeader)

      const packageName = encodeURIComponent(name)
      const raw = await api.get<TPackageData[]>(`packages/${packageName}`)

      return raw.map(data => data.map(toPackage))
    },
    async delete(name: string, version: string): Promise<HttpResponse> {
      const api = await getApi(type, getTokenHeader)

      const packageName = encodeURIComponent(name)

      return api.delete(`packages/${packageName}/${version}`)
    },
  }
}

async function getApi(
  type: ProjectType,
  getTokenHeader: (token: string) => Record<string, string>,
): Promise<HttpClient> {
  await when(() => Boolean(context.getState().auth.user.data))

  const { servers } = context.getState().startup
  const { apiToken } = context.getState().auth.user.data!

  const server = servers[type]

  if (!server) throw new Error('Server is not registered')

  return httpClientFactory({
    url: server,
    init: {
      headers: getTokenHeader(apiToken),
    },
  })
}
