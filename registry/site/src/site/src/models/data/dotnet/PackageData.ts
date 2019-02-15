import Package from '../../view/dotnet/Package'
import PackageDependency from '../../view/dotnet/PackageDependency'
import BasePackageData, { toPackage as toBasePackage } from '../PackageData'

export default interface PackageData extends BasePackageData {
  dependencies: PackageDependency[]
}

export const toPackage = (data: PackageData): Package => {
  return {
    ...toBasePackage(data),
    dependencies: data.dependencies,
  }
}