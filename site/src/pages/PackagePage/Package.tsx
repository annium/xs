import React from 'react'

import { Package as DotnetPackage } from '../../components/dotnet/Package'
import { Package as NodePackage } from '../../components/node/Package'
import { MetaPackage } from '../../models/view/MetaPackage'
import { ProjectType } from '../../models/view/ProjectType'
import { UserMetaPackageAccess } from '../../models/view/UserMetaPackageAccess'


type Props = {
  access: UserMetaPackageAccess
  metaPackage: MetaPackage
  version?: string
}

export const Package = ({ access, metaPackage, version }: Props) => {
  switch (metaPackage.type) {
    case ProjectType.Dotnet:
      return <DotnetPackage access={access} metaPackage={metaPackage} version={version} />
    case ProjectType.Node:
      return <NodePackage access={access} metaPackage={metaPackage} version={version} />
    default:
      return <span />
  }
}
