import { authHeader, apiUrl, handleResponse } from '../_helpers';

export const captureService = {
    start,
    stop
};

const requestOptions = {
    method: 'POST',
    headers: authHeader()
};

async function start(serviceCode) {
    const response = await fetch(`${apiUrl}/record/start/${serviceCode}`, requestOptions);

    return handleResponse(response);
}

async function stop(serviceCode) {
    const response = await fetch(`${apiUrl}/record/stop/${serviceCode}`, requestOptions);

    return handleResponse(response);
}
