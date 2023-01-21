import * as utils from '@annium/utils'
import Col from 'antd/lib/col'
import message from 'antd/lib/message'
import Row from 'antd/lib/row'
import React, {
  Dispatch,
  SetStateAction,
  useCallback,
  useEffect,
  useState,
} from 'react'
import { RouteComponentProps } from 'react-router'

import * as metaPackagesApi from '../../api/metaPackages'
import { PackageFilter } from '../../components/PackageFilter'
import { PackageList } from '../../components/PackageList'
import { MetaPackage } from '../../models/view/MetaPackage'
import { ProjectType } from '../../models/view/ProjectType'
import { getCenteredLayout } from '../../utils/layout'

import styles from './index.module.scss'

type Props = RouteComponentProps

export const PackagesPage = ({ history, location }: Props) => {
  const [packages, setPackages] = useState<MetaPackage[]>([])
  const params = new URLSearchParams(location.search)
  const [type, setType] = useState<ProjectType>(
    Object.values(ProjectType).includes(params.get('type'))
      ? (params.get('type') as ProjectType)
      : ProjectType.Any,
  )
  const [query, setQuery] = useState(params.get('query') || '')
  const runSearch = useCallback(
    () => search(history, type, query, setPackages),
    [history, type, query, setPackages],
  )

  useEffect(() => {
    runSearch()
  }, [runSearch])

  return (
    <div className={styles.page}>
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
    </div>
  )
}

const search = async (
  history: Props['history'],
  type: ProjectType,
  query: string,
  setPackages: Dispatch<SetStateAction<MetaPackage[]>>,
) => {
  utils.history.updateLocation(history, { type, query })
  const packagesResult = await metaPackagesApi.search('', type, query, 1)

  if (packagesResult.isSuccess) setPackages(packagesResult.data)
  else
    message.error(
      `Packages load failed with: ${packagesResult.plainErrors.join(', ')}`,
    )
}
