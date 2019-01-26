export default interface Response<T> {
  data: T | null
  error: string | null
}