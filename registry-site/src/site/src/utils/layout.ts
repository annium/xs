import { ColSize } from 'antd/lib/grid'

export function getCenteredLayout(
  xs: number,
  sm: number,
  md: number,
  lg: number,
  xl: number
): { [key: string]: ColSize } {
  return {
    xs: { offset: (24 - xs) / 2, span: xs },
    sm: { offset: (24 - sm) / 2, span: sm },
    md: { offset: (24 - md) / 2, span: md },
    lg: { offset: (24 - lg) / 2, span: lg },
    xl: { offset: (24 - xl) / 2, span: xl },
  }
}
