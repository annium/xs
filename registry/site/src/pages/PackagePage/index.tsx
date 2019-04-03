import Col from 'antd/lib/col'
import message from 'antd/lib/message'
import Row from 'antd/lib/row'
import React, { Dispatch, SetStateAction, useEffect, useState } from 'react'
import { RouteComponentProps, withRouter } from 'react-router'

import * as metaPackagesApi from '../../api/metaPackages'
import { MetaPackage } from '../../models/view/MetaPackage'
import { UserMetaPackageAccess } from '../../models/view/UserMetaPackageAccess'
import { connect, Store } from '../../store'
import { getCenteredLayout } from '../../utils/layout'
import { parseNameVersion } from '../../utils/nameVersion'

import styles from './index.module.scss'
import { Package } from './Package'

type SelectorProps = Partial<Pick<Store, 'auth'>>
type RouteProps = RouteComponentProps<{ type: string, nameVersion: string }>
type Props = SelectorProps & RouteProps

export const PackagePage = withRouter(connect<RouteProps, SelectorProps>(
  ({ auth }) => ({ auth }),
  ({ auth, match: { params: { type, nameVersion } } }: Props) => {
    const [metaPackage, setMetaPackage] = useState<MetaPackage>()

    useEffect(() => { loadMetaPackage(type, nameVersion, setMetaPackage) }, [type, nameVersion])

    if (!metaPackage) return null

    const { version } = parseNameVersion(nameVersion)
    const access = new UserMetaPackageAccess(auth!.user.data!.id, metaPackage!.ownerId, metaPackage!.permissions)


    console.warn('RENDER PackagePage')

    return (
      <div className={styles.page}>
        <Row>
          <Col {...getCenteredLayout(22, 22, 20, 18, 14)}>
            <Package access={access} metaPackage={metaPackage} version={version} />
          </Col>
        </Row>
      </div>
    )
  },
))


const loadMetaPackage = async (
  type: string,
  nameVersion: string,
  setMetaPackage: Dispatch<SetStateAction<MetaPackage | undefined>>,
) => {
  const { name } = parseNameVersion(nameVersion)
  const packageResult = await metaPackagesApi.get(type, name)

  if (packageResult.isSuccess)
    setMetaPackage(packageResult.data)
  else
    message.error(`Package load failed with: ${packageResult.plainErrors.join(', ')}`)
}
