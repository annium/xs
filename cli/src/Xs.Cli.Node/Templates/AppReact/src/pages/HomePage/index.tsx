import { Trans } from '@lingui/macro'
import React from 'react'
import { RouteComponentProps } from 'react-router'

import { connect, Store } from '../../store'

import styles from './index.module.scss'


type Props = Pick<Store, 'startup'> & RouteComponentProps

export const HomePage = connect<RouteComponentProps, Pick<Store, 'startup'>>(
  ({ startup }) => ({ startup }),
  ({ startup }: Props) => {
    const { location } = startup

    return (
      <div className={styles.page}>
        <Trans>Started at {`${location.pathname}${location.search}`}</Trans>
      </div>
    )
  },
)
