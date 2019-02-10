import Avatar from 'antd/lib/avatar'
import Button from 'antd/lib/button'
import Dropdown from 'antd/lib/dropdown'
import Menu, { ClickParam } from 'antd/lib/menu'
import * as React from 'react'

import { ProjectType } from '../../models/view/ProjectType'

import styles from './index.module.scss'


type Props = {
  type: ProjectType
  onSelect: (type: ProjectType) => void
}

export default class ProjectTypeSelect extends React.Component<Props> {
  private static readonly keys: (keyof typeof ProjectType)[] = Object.keys(ProjectType)
    .filter(key => typeof key === 'string')
    .map(key => key as keyof typeof ProjectType)

  render() {
    const { type } = this.props

    return (
      <Dropdown overlay={this.renderMenu()} trigger={['click']}>
        <Button>{this.renderItem(type)}</Button>
      </Dropdown>
    )
  }

  private renderMenu = () => {
    return (
      <Menu onClick={this.handleMenuClick}>
        {ProjectTypeSelect.keys.map(key => (
          <Menu.Item key={key}>
            {this.renderItem(ProjectType[key])}
          </Menu.Item>
        ))}
      </Menu>
    )
  }

  private renderItem(type: ProjectType) {
    return (
      <div className={styles.item}>
        {type ? <Avatar src={`/icons/${type}.svg`} size="small" /> : <Avatar icon="question" size={24} />}
        <span className={styles.label}>
          {ProjectTypeSelect.keys.find(key => ProjectType[key] === type)}
        </span>
      </div>
    )
  }

  private handleMenuClick = (param: ClickParam) => {
    this.props.onSelect(ProjectType[param.key as keyof typeof ProjectType])
  }
}