import BasePackage from '../Package'

import PackageDependency from './PackageDependency'


export default interface Package extends BasePackage {
  dependencies: PackageDependency[]
}