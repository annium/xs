import { Trans } from '@lingui/macro'
import { observer } from 'mobx-react-lite'
import React from 'react'

import { useStore } from '../../stores'

import { useStyles } from './styles'


export const HomePage = observer(() => {
  const styles = useStyles()
  const location = useStore().startup.location

  return (
    <div className={styles.page}>
      <Trans>Started at {`${location.pathname}${location.search}`}</Trans>
    </div>
  )
})
