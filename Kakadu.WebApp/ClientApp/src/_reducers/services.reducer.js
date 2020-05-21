import { serviceConstants } from '../_constants';

export function services(state = {}, action) {
  switch (action.type) {
    case serviceConstants.GETALL_REQUEST:
      return {
        loading: true
      };
    case serviceConstants.GETALL_SUCCESS:
      return {
        items: action.services
      };
    case serviceConstants.GETALL_FAILURE:
      return { 
        error: action.error
      };
    case serviceConstants.GETBYCODE_REQUEST:
      return {
        loading: true
      };
    case serviceConstants.GETBYCODE_SUCCESS:
      return {
        items: action.service
      };
    case serviceConstants.GETBYCODE_FAILURE:
      return {
        error: action.error
      };
    default:
      return state
  }
}