import React from 'react'

import DotnetPackage from '../../components/dotnet/Package'
import NodePackage from '../../components/node/Package'
import MetaPackage from '../../models/view/MetaPackage'
import { ProjectType } from '../../models/view/ProjectType'


type Props = {
  metaPackage: MetaPackage
}

export default function Package({ metaPackage }: Props) {
  switch (metaPackage.type) {
    case ProjectType.Dotnet:
      return <DotnetPackage metaPackage={metaPackage} />
    case ProjectType.Node:
      return <NodePackage metaPackage={metaPackage} />
    default:
      return null
  }
}