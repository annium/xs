import Col from 'antd/lib/col'
import message from 'antd/lib/message'
import Row from 'antd/lib/row'
import React, { Dispatch, SetStateAction, useEffect, useState } from 'react'
import { RouteComponentProps } from 'react-router'

import * as metaPackagesApi from '../../api/metaPackages'
import { PackageFilter } from '../../components/PackageFilter'
import { PackageList } from '../../components/PackageList'
import { MetaPackage } from '../../models/view/MetaPackage'
import { ProjectType } from '../../models/view/ProjectType'
import { inject, Store } from '../../store'
import { updateLocation } from '../../utils/history'
import { getCenteredLayout } from '../../utils/layout'

import styles from './index.module.scss'


type Props = Partial<Pick<Store, 'user'>> & RouteComponentProps

export const HomePage = inject<RouteComponentProps, Pick<Store, 'user'>>(
  ({ user }) => ({ user }),
  ({ user, history, location }: Props) => {
    const [packages, setPackages] = useState<MetaPackage[]>([])
    const params = new URLSearchParams(location.search)
    const [type, setType] = useState<ProjectType>(Object.values(ProjectType).includes(params.get('type'))
      ? params.get('type') as ProjectType
      : ProjectType.Any)
    const [query, setQuery] = useState(params.get('query') || '')
    const runSearch = () => search(user, history, type, query, setPackages)

    useEffect(() => { runSearch() }, [])

    return (
      <div className={styles.page} >
        <Row className={styles.row}>
          <Col className={styles.col} {...getCenteredLayout(22, 22, 20, 18, 14)}>
            <h1>My packages</h1>
            <PackageFilter
              type={type}
              onTypeChange={setType}
              query={query}
              onQueryChange={setQuery}
              onSubmit={runSearch}
            />
            <PackageList packages={packages} />
          </Col>
        </Row>
      </div >
    )
  },
)

const search = async (
  user: Props['user'],
  history: Props['history'],
  type: ProjectType,
  query: string,
  setPackages: Dispatch<SetStateAction<MetaPackage[]>>,
) => {
  updateLocation(history, { type, query })
  const packagesResult = await metaPackagesApi.search(user!.data!.id, type, query, 1)

  if (packagesResult.isSuccess)
    setPackages(packagesResult.data)
  else
    message.error(`Packages load failed with: ${packagesResult.error}`)
}
