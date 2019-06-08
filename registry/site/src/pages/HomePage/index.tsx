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
import { connect } from '../../store'
import { getCenteredLayout } from '../../utils/layout'

import styles from './index.module.scss'

type OwnProps = RouteComponentProps
type SelectorProps = { ownerId: string }
type Props = OwnProps & SelectorProps

export const HomePage = connect<OwnProps, SelectorProps>(
  ({ auth }) => ({ ownerId: auth.user.data ? auth.user.data.id : '' }),
  ({ ownerId, history, location }: Props) => {
    const [packages, setPackages] = useState<MetaPackage[]>([])
    const params = new URLSearchParams(location.search)
    const [type, setType] = useState<ProjectType>(
      Object.values(ProjectType).includes(params.get('type'))
        ? (params.get('type') as ProjectType)
        : ProjectType.Any,
    )
    const [query, setQuery] = useState(params.get('query') || '')
    const runSearch = useCallback(
      () => search(ownerId, type, query, setPackages, history),
      [ownerId, type, query, setPackages, history],
    )

    useEffect(() => {
      runSearch()
    }, [runSearch])

    return (
      <div className={styles.page}>
        <Row className={styles.row}>
          <Col
            className={styles.col}
            {...getCenteredLayout(22, 22, 20, 18, 14)}
          >
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
  },
)

const search = async (
  ownerId: string,
  type: ProjectType,
  query: string,
  setPackages: Dispatch<SetStateAction<MetaPackage[]>>,
  history: Props['history'],
) => {
  utils.history.updateLocation(history, { type, query })
  const packagesResult = await metaPackagesApi.search(ownerId, type, query, 1)

  if (packagesResult.isSuccess) setPackages(packagesResult.data)
  else
    message.error(
      `Packages load failed with: ${packagesResult.plainErrors.join(', ')}`,
    )
}
