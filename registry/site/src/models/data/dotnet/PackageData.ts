import { Package } from '../../view/dotnet/Package'
import { PackageDependency } from '../../view/dotnet/PackageDependency'
import { PackageData as BasePackageData, toPackage as toBasePackage } from '../PackageData'

export type PackageData = BasePackageData & {
  dependencies: PackageDependency[]
}

export const toPackage = (data: PackageData): Package => ({
  ...toBasePackage(data),
  dependencies: data.dependencies,
})
