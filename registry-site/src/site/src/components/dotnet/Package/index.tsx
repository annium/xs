import React from 'react'

import MetaPackage from '../../../models/view/MetaPackage'

type Props = {
  metaPackage: MetaPackage
}

export default class Package extends React.PureComponent<Props>{
  render() {
    const { metaPackage } = this.props

    return (
      <div>{metaPackage.type}: {metaPackage.name}</div>
    )
  }
}