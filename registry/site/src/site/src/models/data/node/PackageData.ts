import Package from '../../view/node/Package'
import PackageDependency from '../../view/node/PackageDependency'
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