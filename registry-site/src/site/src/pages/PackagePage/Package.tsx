import React from 'react'

import DotnetPackage from '../../components/dotnet/Package'
import NodePackage from '../../components/node/Package'
import MetaPackage from '../../models/view/MetaPackage'
import { ProjectType } from '../../models/view/ProjectType'
import UserMetaPackageAccess from '../../models/view/UserMetaPackageAccess'


type Props = {
  access: UserMetaPackageAccess
  metaPackage: MetaPackage
  version?: string
}

export default function Package({ access, metaPackage, version }: Props) {
  switch (metaPackage.type) {
    case ProjectType.Dotnet:
      return <DotnetPackage access={access} metaPackage={metaPackage} version={version} />
    case ProjectType.Node:
      return <NodePackage access={access} metaPackage={metaPackage} version={version} />
    default:
      return null
  }
}