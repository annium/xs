import PackageData, { toPackage } from '../../models/data/dotnet/PackageData'
import Package from '../../models/view/dotnet/Package'
import { ProjectType } from '../../models/view/ProjectType'

import createApi from '.'

const api = createApi<PackageData, Package>(
  ProjectType.Dotnet,
  (token: string) => ({ 'X-NuGet-ApiKey': token }),
  toPackage
)

export default api