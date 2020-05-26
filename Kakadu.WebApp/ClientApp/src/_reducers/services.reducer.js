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
        item: action.service
      };
    case serviceConstants.GETBYCODE_FAILURE:
      return {
        error: action.error
      };
    case serviceConstants.UPDATE_REQUEST:
      return {
        loading: true
      };
    case serviceConstants.UPDATE_SUCCESS: 
      return {
        item: action.service
      };
    case serviceConstants.UPDATE_FAILURE:
      return {
        error: action.error
      };
    case serviceConstants.CREATE_REQUEST:
      return {
        loading: true
      };
    case serviceConstants.CREATE_SUCCESS:
      return {
        item: action.service
      };
    case serviceConstants.CREATE_FAILURE:
      return {
        error: action.error
      };
    default:
      return state
  }
}