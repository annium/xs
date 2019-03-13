import Icon from 'antd/lib/icon'
import Spin from 'antd/lib/spin'
import cx from 'classnames'
import * as React from 'react'

import styles from './index.module.scss'


export type Props = {
  isLoading: boolean
  className?: string
  size?: 'big' | 'normal' | 'small'
}

export class Loader extends React.Component<Props> {
  public render() {
    const { className, isLoading } = this.props

    const cls = cx(styles.loader, className)
    const children = React.Children.count(this.props.children) ? this.props.children : <span />

    if (!isLoading)
      return (
        <div className={cls}>
          {children}
        </div>
      )

    return (
      <Spin className={cls} wrapperClassName={cls} indicator={this.getIndicator()}>
        {children}
      </Spin>
    )
  }

  private getSize() {
    const { size } = this.props

    switch (size) {
      case 'big':
        return 4
      case 'small':
        return 2
      default:
        return 3
    }
  }

  private getIndicator() {
    const size = this.getSize()
    const toRem = (value: number) => `${value}rem`
    const style = {
      fontSize: toRem(size),
      height: toRem(size),
      marginLeft: toRem(-size / 2),
      marginTop: toRem(-size / 2),
      width: toRem(size),
    }

    return <Icon type="sync" style={style} spin={true} />
  }
}
