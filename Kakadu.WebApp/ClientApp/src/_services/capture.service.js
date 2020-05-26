import { authHeader, apiUrl } from '../_helpers';

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

function handleResponse(response) {
    return response.text().then(text => {
        const data = text && JSON.parse(text);
        if (!response.ok) {
            
            const error = { 
                response: (data && data.Message) || response.url + ' - ' + response.statusText
            };
            return Promise.reject(error);
        }

        return data;
    });
}