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
      case serviceConstants.REMOVE_REQUEST:
        return {
          ...state,
          items: state.items.map(service =>
            service.code === action.serviceCode
              ? { ...service, deleting: true }
              : service
          )
        };
      case serviceConstants.REMOVE_SUCCESS:
        return {
          items: state.items.filter(service => service.code !== action.serviceCode)
        };
      case serviceConstants.REMOVE_FAILURE:
        return {
          ...state,
          items: state.items.map(service => {
            if (service.code === action.serviceCode) {
              const { deleting, ...serviceCopy } = service;
              return { ...serviceCopy, deleteError: action.error };
            }

            return service;
          })
        };
    default:
      return state
  }
}