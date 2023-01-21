import { Col, Row } from 'antd/lib/grid'
import message from 'antd/lib/message'
import confirm from 'antd/lib/modal/confirm'
import { chain } from 'lodash'
import React, { Dispatch, SetStateAction, useEffect, useState } from 'react'

import { api as serverApi } from '../../api/server/node'
import { MetaPackage } from '../../models/view/MetaPackage'
import { Package as PackageModel } from '../../models/view/node/Package'
import { UserMetaPackageAccess } from '../../models/view/UserMetaPackageAccess'
import { gutter } from '../../utils/layout'
import { PackageInfo } from '../PackageInfo'
import { PackageOwner } from '../PackageOwner'
import { PackagePermissions } from '../PackagePermissions'
import { PackageStats } from '../PackageStats'
import { PackageTitle } from '../PackageTitle'
import { PackageVersions } from '../PackageVersions'

import { Dependencies } from './Dependencies'


type Props = {
  access: UserMetaPackageAccess
  metaPackage: MetaPackage
  version?: string
}

export const Package = ({ access, metaPackage, version }: Props) => {
  const [packages, setPackages] = useState<PackageModel[]>([])

  const pkg: PackageModel = version ?
    packages.filter(p => p.version === version)[0] :
    chain(packages).sortBy((p: PackageModel) => p.version).value()[packages.length - 1]

  useEffect(() => { loadPackages(metaPackage.name, setPackages) }, [metaPackage.name])

  if (!pkg) return null

  return (
    <Row gutter={gutter}>
      <Col span={16}>
        <PackageTitle access={access} metaPackage={metaPackage} pkg={pkg} onDelete={handleDelete(pkg)} />
        <Dependencies dependencies={pkg.dependencies} />
        <PackageVersions type={metaPackage.type} pkg={pkg} packages={packages} />
      </Col>
      <Col span={8}>
        <PackageInfo pkg={pkg} />
        {access.isOwner ? <PackagePermissions metaPackage={metaPackage} /> : undefined}
        <PackageStats pkg={pkg} packages={packages} />
        <PackageOwner metaPackage={metaPackage} />
      </Col>
    </Row>
  )
}

const loadPackages = async (name: string, setPackages: Dispatch<SetStateAction<PackageModel[]>>) => {
  const packagesResult = await serverApi.get(name)

  if (packagesResult.isSuccess)
    setPackages(packagesResult.data)
  else
    message.error(`Package load failed with: ${packagesResult.plainErrors.join(', ')}`)
}

const handleDelete = ({ name, version }: PackageModel) => () => confirm({
  title: 'Confirm delete',
  content: <span>Confirm, if you really want to delete package <b>{name} {version}</b></span>,
  onOk: () => serverApi.delete(name, version)
    .then(() => message.success('Package successfully deleted'))
    .catch(error => message.error(`Package deletion failed with: ${error}`)),
  maskClosable: true,
})
