import PackageData, { toPackage } from '../../models/data/node/PackageData'
import Package from '../../models/view/node/Package'
import { ProjectType } from '../../models/view/ProjectType'

import { createApi } from '.'

export const api = createApi<PackageData, Package>(
  ProjectType.Node,
  (token: string) => ({ Authorization: `Bearer ${token}` }),
  toPackage,
)
